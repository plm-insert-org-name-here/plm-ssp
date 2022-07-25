from utils.types import PacketType, Template, Params
from runner import Runner
from sock import Sock

sock = Sock('/tmp/plm-ssp.sock')
runners = {}

def print_params(ps):
    print(f'job type: {ps.job_type}')
    for t in ps.templs:
        print(f'templ: id: {t.id} x: {t.x} y: {t.y} w: {t.w} h: {t.h} order: {t.order}')

def print_frame(frame):
    print(f'frame shape: {frame.shape}')

def read_params():
    detector_id = sock.read_int(4)
    job_type = sock.read_int(4)
    templ_count = sock.read_int(4)
    templs = []
    for i in range(templ_count):
        id = sock.read_int(4)
        x = sock.read_int(4)
        y = sock.read_int(4)
        w = sock.read_int(4)
        h = sock.read_int(4)
        order = sock.read_int(4)
        templs.append(Template(id, x, y, w, h, order))

    params = Params(job_type, templs)
    # print_params(params)
    return detector_id, params

def read_frame():
    detector_id = sock.read_int(4)
    frame_size = sock.read_int(4)
    frame = sock.read_frame(frame_size)
    # print_frame(frame)
    return detector_id, frame

def process_packet():
    packet_type = sock.read_int(4)

    if packet_type == PacketType.Params.value:
        detector_id, params = read_params()
        try:
            runners[detector_id].stop()
        except KeyError:
            pass
        runner = Runner(detector_id, params, sock)
        runners[detector_id] = runner
        runner.start()

    elif packet_type == PacketType.Frame.value:
        detector_id, frame = read_frame()
        try:
            runners[detector_id].enqueue_frame(frame)
        except KeyError:
            # TODO(rg): handle error
            # processor should not receive a frame for a given task before the params have been
            # sent
            print('error: frame received, but no params')
            pass

    else:
        print(f'invalid packet type: {packet_type}')

while True:
    process_packet()
