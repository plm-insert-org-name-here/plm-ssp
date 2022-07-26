from utils.queue import Queue
from threading import Thread, Event
from algorithms import get_algorithm

class Runner:
    def __init__(self, detector_id, params, sock, dummy):
        self._queue = Queue(4)
        self._detector_id = detector_id
        self._params = params
        self._sock = sock
        self._stopped_event = Event()
        self._algorithm = get_algorithm(params, dummy)
        self._thread = None

    def _run(self):
        while True:
            if self._stopped_event.is_set():
                break

            with self._queue.get_latest() as frame:
                result = self._algorithm.run(frame)
                self._sock.send_result(
                        self._detector_id, 
                        self._params.job_type, 
                        result)

    def enqueue_frame(self, frame):
        self._queue.insert(frame)

    def start(self):
        if self._thread is None:
            self._thread = Thread(target=self._run)
            self._thread.start()

    def stop(self):
        self._stopped_event.set()
        if self._thread is not None:
            self._thread.join()
            self._thread = None
