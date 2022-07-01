from threading import Condition

class ReadWriteLock:

    def __init__(self):
        self._accept_read = Condition()
        self._accept_write = Condition()
        self.readers = 0
        self._writer = False 

    def acquire_read(self):
        with self._accept_read:
            while self._writer:
                self._accept_read.wait()
            self.readers += 1

    def release_read(self):
        with self._accept_write:
            self.readers -= 1
            if not self.readers:
                self._accept_write.notify()

    def acquire_write(self):
        with self._accept_write:
            while self.readers > 0 or self._writer:
                self._accept_write.wait()
            self._writer = True

    def release_write(self):
        with self._accept_read, self._accept_write:
            self._writer = False
            self._accept_read.notify_all()
            self._accept_write.notify()

class ReadRWLock:
    def __init__(self, rwLock):
        self.rwLock = rwLock

    def __enter__(self):
        self.rwLock.acquire_read()
        return self

    def __exit__(self, exc_type, exc_value, traceback):
        self.rwLock.release_read()
        return False

class WriteRWLock:
    def __init__(self, rwLock):
        self.rwLock = rwLock

    def __enter__(self):
        self.rwLock.acquire_write()
        return self

    def __exit__(self, exc_type, exc_value, traceback):
        self.rwLock.release_write()
        return False
