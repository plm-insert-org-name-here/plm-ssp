from algorithms.base import TF_classifier
import cv2
import numpy as np
from PIL import Image

class Item_kit_algo(TF_classifier):
    def __init__(self, params):
        super().__init__("seeger_yolov2.tflite")
        self.params = params
        self.image_size = 56

    def prep_image(self, image):
        resized = cv2.resize(image, (self.image_size, self.image_size), interpolation = cv2.INTER_AREA)
        data = np.empty((1, self.image_size, self.image_size, 3))
        data[0] = resized
        
        image = Image.fromarray(image, 'RGB')
        input_data_type = self._input_details[0]["dtype"]
        image = np.array(image.resize((self.image_size, self.image_size)), dtype=input_data_type)
        input_tensor = np.empty((1, self.image_size, self.image_size, 3), dtype=np.float32)
        input_tensor[0] = image
        if input_data_type == np.float32:
            input_tensor = input_tensor / 255.

        if input_tensor.shape == (1, self.image_size, self.image_size):
            input_tensor = np.stack(input_tensor*3, axis=0)

        return input_tensor

    def adapt(self, result):
        probabilities = np.array(result[0])        
        labels = [False, True, None]
        output_neuron = np.argmax(probabilities)         
        
        return labels[output_neuron], probabilities[output_neuron]
