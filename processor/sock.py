import socket
import cv2 as cv
import numpy as np
from utils.serialization import to_bytes, from_bytes
from utils.types import PacketType, Template, Params, JobType
from threading import Lock

class Sock:
    def __init__(self, req_sock_path, res_sock_path):
        self._req_sock = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
        self._req_sock.connect(req_sock_path)
        # NOTE(rg): only one thread/process accesses req_sock, so no need for locking

        self._res_sock = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
        self._res_sock.connect(res_sock_path)
        self._res_lock = Lock()

    def _read_int(self, n_bytes):
        bytes = self._req_sock.recv(n_bytes)
        return from_bytes(bytes)

    def _write_int(self, number, n_bytes):
        n = to_bytes(number, n_bytes)
        bytes_sent = self._res_sock.send(n)
        return bytes_sent

    def _read_bytes(self, n_bytes):
        return self._req_sock.recv(n_bytes)

    def _write_bytes(self, bs):
        bytes_sent = self._res_sock.send(bs)
        return bytes_sent
    
    def read_packet_type(self):
        return self._read_int(4)

    def read_params(self):
        detector_id = self._read_int(4)
        task_id = self._read_int(4)
        job_type = self._read_int(4)

        if job_type == JobType.QA.value:
            params = Params(job_type, None)
        else:
            templ_count = self._read_int(4)
            templs = []
            for i in range(templ_count):
                id = self._read_int(4)
                x = self._read_int(4)
                y = self._read_int(4)
                w = self._read_int(4)
                h = self._read_int(4)
                order = self._read_int(4)
                templs.append(Template(id, x, y, w, h, order))

            params = Params(job_type, templs)

        return detector_id, task_id, params

    def read_frame(self):
        detector_id = self._read_int(4)
        frame_size = self._read_int(4)

        bytes = self._read_bytes(frame_size)
        bytes = np.frombuffer(bytes, np.byte)
        frame = cv.imdecode(bytes, cv.IMREAD_GRAYSCALE)
        return detector_id, frame

    def read_stop(self):
        task_id = self._read_int(4)
        return task_id

    def read_pause(self):
        task_id = self._read_int(4)
        return task_id

    def send_result(self, detector_id, task_id, job_type, result):
        with self._res_lock:
            self._write_int(detector_id, 4)
            self._write_int(task_id, 4)
            self._write_int(job_type, 4)
            self._write_bytes(result.serialize())
