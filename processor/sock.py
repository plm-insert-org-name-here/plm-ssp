import socket
import cv2 as cv
import numpy as np
from utils.serialization import to_bytes, from_bytes
from threading import Lock

class Sock:
    def __init__(self, path):
        self._sock = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
        self._sock.connect(path)
        self._lock = Lock()

    def read_int(self, n_bytes):
        with self._lock:
            bytes = self._sock.recv(n_bytes)

        return from_bytes(bytes)

    def write_int(self, number, n_bytes):
        n = to_bytes(number, n_bytes)
        print(n)
        with self._lock:
            bytes_sent = self._sock.send(n)
            return bytes_sent

    def write_bytes(self, bs):
        print(bs)
        with self._lock:
            bytes_sent = self._sock.send(bs)
            return bytes_sent

    def read_frame(self, n_bytes):
        with self._lock:
            bytes = self._sock.recv(n_bytes)

        bytes = np.frombuffer(bytes, np.byte)
        # TODO: might need IMREAD_UNCHANGED?
        frame = cv.imdecode(bytes, cv.IMREAD_GRAYSCALE)
        return frame

    def send_result(self, detector_id, job_type, result):
        self.write_int(detector_id, 4)
        self.write_int(job_type, 4)
        self.write_bytes(result.serialize())
