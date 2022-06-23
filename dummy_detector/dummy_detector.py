# https: 9696
# http: 9695

import asyncio
import websockets
import ssl
import logging
import cv2
from time import sleep

# logger = logging.getLogger("asyncio")

def get_snapshot():
    camera = cv2.VideoCapture(0)
    camera.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
    camera.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)

    # Capture a couple frames before taking the snapshot to make sure
    # the camera exposure or whatever is stabilized
    for _ in range(10):
        camera.read()

    s, image = camera.read()
    if not s:
        raise Exception('Cannot acquire image')
    success, image_bmp = cv2.imencode(".bmp", image)
    if not success:
        raise Exception('Cannot encode to BMP')

    return bytes(image_bmp)

def get_response(cmd):
    if cmd == 'Ping':
        return 'Pong'
    else:
        return 'Ok'

async def hello():
    async with websockets.connect("wss://localhost:9696/detectors/controller", \
            ssl=ssl._create_unverified_context()) as websocket:
        await websocket.send("121212121212")
        while True:
            try:
                cmd = await websocket.recv()
                await websocket.send(get_response(cmd))
                if cmd == "TakeSnapshot":
                    await websocket.send(get_snapshot())

            except websockets.exceptions.ConnectionClosedError as ex:
                print(ex)
                break



asyncio.run(hello())
