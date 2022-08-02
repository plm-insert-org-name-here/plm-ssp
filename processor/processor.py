import argparse

parser = argparse.ArgumentParser(description="PLM server side processing - processor")
parser.add_argument(
        '--dummy',
        nargs='?',
        const=False,
        default=None,
        help="Use dummy algorithms. If no argument is entered, the algorithms periodically produce\
        the results preset in code. If the argument is a filename of a script inside the\
        dummy_scripts/ folder, the script will be run")

args = parser.parse_args()

from utils.types import PacketType, Template, Params, JobType
from runner import Runner
from sock import Sock

sock = Sock('/tmp/plm-ssp-req.sock', '/tmp/plm-ssp-res.sock')
runners = {}

def print_params(ps):
    print(f'job type: {ps.job_type}')
    for t in ps.templs:
        print(f'templ: id: {t.id} x: {t.x} y: {t.y} w: {t.w} h: {t.h} order: {t.order}')

def print_frame(frame):
    print(f'frame shape: {frame.shape}')

def process_packet():
    packet_type = sock.read_packet_type()

    if packet_type == PacketType.Params.value:
        detector_id, task_id, params = sock.read_params()
        try:
            runners[detector_id].stop()
        except KeyError:
            pass
        runner = Runner(detector_id, task_id, params, sock, args.dummy)
        runners[detector_id] = runner
        runner.start()

    elif packet_type == PacketType.Frame.value:
        detector_id, frame = sock.read_frame()
        try:
            runners[detector_id].enqueue_frame(frame)
        except KeyError:
            # TODO(rg): handle error
            # processor should not receive a frame for a given task before the params have been
            # sent
            print('error: frame received, but no params')
            pass

    elif packet_type == PacketType.Stop.value:
        detector_id = sock.read_stop()
        runners[detector_id].stop()

    else:
        print(f'invalid packet type: {packet_type}')

while True:
    process_packet()
