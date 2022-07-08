import socket
from collections import namedtuple

sock = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
sock.connect("/tmp/plm-ssp.sock")

Templ = namedtuple("Templ", "id x y w h")
Params = namedtuple("Params", "job_type detector_id templs")

def print_params(ps):
        print(f'job type: {ps.job_type}')
        print(f'detector id: {ps.detector_id}')
        for t in ps.templs:
            print(f'templ: id: {t.id} x: {t.x} y: {t.y} w: {t.w} h: {t.h}')

def read_int_from_sock(n_bytes):
    bytes = sock.recv(n_bytes)
    return int.from_bytes(bytes, byteorder='little')

def read_req():
    pass

def read_params():
    job_type = read_int_from_sock(1)
    detector_id = read_int_from_sock(4)
    templ_count = read_int_from_sock(4)
    templs = []
    for i in range(templ_count):
        id = read_int_from_sock(4)
        x = read_int_from_sock(4)
        y = read_int_from_sock(4)
        w = read_int_from_sock(4)
        h = read_int_from_sock(4)
        templs.append(Templ(id, x, y, w, h))

    params = Params(job_type, detector_id, templs)
    print_params(params)

def read_pkg():
    msg_type = read_int_from_sock(4)

    if msg_type == 0:
        read_params()
    elif msg_type == 1:
        print('request')
    else:
        print('unknown')

# while True:
pkg = read_pkg()

