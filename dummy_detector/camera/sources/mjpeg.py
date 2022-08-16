import os
import cv2
from camera.camera_error import CameraError
from camera.sources import BaseCamera
from time import sleep, time
from math import ceil
import ffmpeg

class MjpegCamera(BaseCamera):

    def __init__(self, file, fps_override):
        super().__init__()
        self.file = file
        self.fps_override = fps_override
        self.current_position = 0
        self.cap = None

    def start(self):
        self._open_file()

    def stop(self):
        self._close_file()

    def acquire(self):
        next = int(self.cap.get(cv2.CAP_PROP_POS_FRAMES))
        # print(f'Next: {next}, Count: {self.frame_count}, eq: {next == self.frame_count}')
        if next == self.frame_count:
            self.cap.set(cv2.CAP_PROP_POS_FRAMES, 0)

        ret, frame = self.cap.read()
        timestamp = time()
        sleep(self.frame_time)
        if not ret:
            raise CameraError(f"Failed to capture frame (position: {next})")
           
        return frame, timestamp


    # NOTE(rg): sometimes, the frame count cannot be decoded from the video metadata. In this case,
    # opencv simply multiplies the video length with the nomimal fps, which may result in a wrong value
    # (e.g. when recording 6 seconds of webcam output into an .avi file using ffmpeg, opencv
    # assumed the frame count was 180 (6 * 30fps). Actually it was 46, since my webcam's fps is
    # rather low)
    # TODO(rg): test properly
    def _determine_actual_frame_count(self):
        # check if the frame count determined by opencv is correct
        total = int(self.cap.get(cv2.CAP_PROP_FRAME_COUNT))
        self.cap.set(cv2.CAP_PROP_POS_FRAMES, total)
        if self.cap.grab():
            return total

        # binary search using the boolean return value of cv2.VideoCapture.grab() to find
        # the last video frame that can be read. That number is the actual frame count. 
        # This presumes that all frames leading up to the final frame can be read 
        prev = total
        curr = int(prev / 2)

        prev_can_read = False

        while True:
            self.cap.set(cv2.CAP_PROP_POS_FRAMES, curr)
            can_read = self.cap.grab()

            diff = abs(prev - curr)

            if diff == 1:
                if prev_can_read:
                    return prev
                if can_read:
                    return curr

            prev_can_read = can_read
            prev = curr
            if can_read:
                curr += ceil(diff / 2)
            else:
                curr -= ceil(diff / 2)

    # There is no consistent way of determining video duration with opencv, so we're using
    # ffmpeg instead
    # TODO(rg): test properly
    def _determine_video_duration(self):
        probe = ffmpeg.probe(self.file)
        return float(probe['streams'][0]['duration'])

    def _open_file(self):

        if self.cap is None:
            self.cap = cv2.VideoCapture(self.file)
            if not self.cap.isOpened():
                raise CameraError("Error opening video file: " + self.file)

            self.frame_count = self._determine_actual_frame_count()
            duration = self._determine_video_duration()

            fps = None
            if self.fps_override:
                fps = self.fps_override
            else:
                fps = self.frame_count / duration

            self.frame_time = 1 / fps

            # Reset to beginning of file
            self.cap.set(cv2.CAP_PROP_POS_AVI_RATIO, 0)

    def _close_file(self):
        if self.cap is None:
            raise CameraError('File is not open, cannot close')
        self.cap.release()
        self.cap = None

