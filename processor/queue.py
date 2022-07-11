from threading import Lock, Event
from contextlib import contextmanager

# TODO(rg): maybe separate lists for payload & locks?
class Frame:
    self.lock = Lock()
    self.payload = None

# TODO(rg): optimal buffer size?
# Ring buffer with random access
# Concurrency support for 1 reader & 1 writer
class Queue:
    def __init__(self, buffer_size):
        self._buffer_size = buffer_size
        self._buffer = [Frame() for i in range(buffer_size)]
        self._latest_index = -1
        self._reading_index = None
        self._first_ready = Event()

    def _get_next_index(self):
        n = (self._latest_index + 1) % self._buffer_size
        if n == self._reading_index:
            n = (self._latest_index + 1) % self._buffer_size
        return n

    def insert(new_payload):
        new_latest_index = _get_next_index()
        frame = self._buffer[new_latest_index]

        with frame.lock:
            frame.payload = new_payload
            self._latest_index = new_latest_index

        self._first_ready.set()

    @contextmanager
    def get_latest():
        self._first_ready.wait()

        self._reading_index = self._latest_index
        frame = self._buffer[self._reading_index]

        with frame.lock:
            yield frame.payload

        self._reading_index = None
