from threading import Lock, Event
from contextlib import contextmanager

# TODO(rg): maybe separate lists for payload & locks?
class Slot:
    def __init__(self):
        self.lock = Lock()
        self.payload = None

# Ring buffer with random access
# Concurrency support for 1 reader & 1 writer
# TODO(rg): optimal buffer size?
class Queue:
    def __init__(self, buffer_size):
        self._buffer_size = buffer_size
        self._buffer = [Slot() for i in range(buffer_size)]
        self._latest_index = -1
        self._reading_index = None
        self._first_ready = Event()

    def _get_next_index(self):
        n = (self._latest_index + 1) % self._buffer_size
        if n == self._reading_index:
            n = (n + 1) % self._buffer_size
        return n

    def insert(self, new_payload):
        new_latest_index = self._get_next_index()
        slot = self._buffer[new_latest_index]

        with slot.lock:
            slot.payload = new_payload
        self._latest_index = new_latest_index

        self._first_ready.set()

    @contextmanager
    def get_latest(self):
        self._first_ready.wait()

        self._reading_index = self._latest_index
        slot = self._buffer[self._reading_index]

        with slot.lock:
            yield slot.payload

        self._reading_index = None
