from camera.camera_error import CameraError
from contextlib import contextmanager
from threading import Thread, Event, Lock
from utils.rwlock import ReadWriteLock, ReadRWLock, WriteRWLock
from time import sleep

class Frame:
    def __init__(self):
        self.lock = ReadWriteLock()
        self.payload = None
        self.timestamp = None

class Camera:
    INSTANCE_LOCK = Lock()
    INSTANCE = None

    def __init__(self, buffer_size):

        self._subscribers = 0
        self._subscribers_lock = Lock()

        self._buffer = [Frame() for i in range(buffer_size)]

        self._latest_index = None

        self._should_create = Event()
        self._created = Event()
        self._space_available = Event()

        self._available_indices = list(range(0, buffer_size))
        self._available_indices_lock = Lock()


    def start(self):
        with self._subscribers_lock:
            if self._subscribers == 0:
                self._camera.start()
                self._thread = Thread(target=self._source)
                self._thread.start()
            self._subscribers += 1
        return self

    def stop(self):
        with self._subscribers_lock:
            self._subscribers -= 1
            if self._subscribers == 0:
                self._thread.join()
                self._thread = None
                self._camera.stop()
        return False

    # TODO(rg): error handling
    @staticmethod
    def get_instance(config):
        with Camera.INSTANCE_LOCK:
            if Camera.INSTANCE is None:
                camera_type = config.camera_type
                camera = None

                if "Web" == camera_type:
                    from camera.sources.web_camera import WebCamera
                    camera = WebCamera(
                            config.camera_webcam_source)
                elif "Mjpeg" == camera_type:
                    from camera.sources.mjpeg import MjpegCamera
                    camera = MjpegCamera(
                            config.camera_mjpeg_file,
                            config.camera_mjpeg_fps_override)
                else:
                    raise CameraError('Unknown camera type: ' + camera_type)

                instance = Camera(config.camera_buffer_size)
                instance._camera = camera
                Camera.INSTANCE = instance
            return Camera.INSTANCE

    # TODO(rg): implement a way to stop the camera
    def _source(self):
        while True:

            go = False
            while not go:
                go = self._should_create.wait(0.5)
                if not self._subscribers:
                    return

            with self._available_indices_lock:
                if not self._available_indices:
                    self._space_available.wait()
                    self._space_available.clear()
                self._latest_index = self._available_indices.pop()

            image, timestamp = self._camera.acquire()
            frame = self._buffer[self._latest_index]
            with WriteRWLock(frame.lock):
                frame.payload = image
                frame.timestamp = timestamp
                self._created.set()

    @contextmanager
    def acquire(self):
        self._should_create.set()
        self._should_create.clear()
        self._created.wait()
        self._created.clear()

        idx = self._latest_index
        frame = self._buffer[idx]

        with ReadRWLock(frame.lock):
            with self._available_indices_lock:
                try:
                    self._available_indices.remove(idx)
                except ValueError:
                    pass

            yield frame.payload, frame.timestamp

        # Only free up the slot when all readers stopped reading from it; 
        # otherwise the writer may pick it and then block on it while there are
        # other, actually unoccupied slots it could write to
        with self._available_indices_lock:
            if not frame.lock.readers:
                self._available_indices.append(idx)
