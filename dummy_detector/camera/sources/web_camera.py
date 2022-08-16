import cv2
from cv2 import VideoCapture
from camera.camera_error import CameraError
from camera.sources import BaseCamera
from time import time

class WebCamera(BaseCamera):

    def __init__(self, source):
        super().__init__()

    def start(self):
        self.camera = VideoCapture(source)
        self.camera.set(cv2.CAP_PROP_FRAME_WIDTH, 640)
        self.camera.set(cv2.CAP_PROP_FRAME_HEIGHT, 480)
        self.camera.set(cv2.CAP_PROP_SATURATION, 0.2)

    def stop(self):
        self.camera.release()
        self.camera = None

    def acquire(self):
        s, image = self.camera.read()
        timestamp = time()
        if not s:
            raise CameraError('Cannot acquire image.')
        return image, timestamp
