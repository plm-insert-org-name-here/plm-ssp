# https: 9696
# http: 9695

import asyncio
import websockets
import ssl
import logging

# logger = logging.getLogger("asyncio")

def get_response(req):
    if req == 'Ping':
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

            except websockets.exceptions.ConnectionClosedError as ex:
                print(ex)
                break



asyncio.run(hello())
