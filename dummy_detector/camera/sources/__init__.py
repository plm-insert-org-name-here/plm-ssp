from camera.camera_error import CameraError

class BaseCamera:
    def __init__(self):
        pass

    def start(self):
        raise CameraError('This method is to be implemented by a subclass.')

    def stop(self):
        raise CameraError('This method is to be implemented by a subclass.')

    def acquire(self):
        raise CameraError('This method is to be implemented by a subclass.')
