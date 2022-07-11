import socket
import cv2 as cv
import numpy as np
from threading import Lock

class Sock:
    def __init__(self, path):
        self._sock = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
        self._sock.connect(path)
        self._lock = Lock()

    def read_int(self, n_bytes):
        with self._lock:
            bytes = self._sock.recv(n_bytes)

        return int.from_bytes(bytes, byteorder='little')

    def read_frame(self, n_bytes):
        with self._lock:
            bytes = self._sock.recv(n_bytes)

        bytes = np.frombuffer(bytes, np.byte)
        # TODO: might need IMREAD_UNCHANGED?
        frame = cv.imdecode(bytes, cv.IMREAD_GRAYSCALE)
        return frame

    def send_result(self, result):
        pass
