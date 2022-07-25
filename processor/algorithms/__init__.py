from utils.types import JobType
from abc import abstractmethod
import tensorflow as tf
import numpy as np
import cv2
from PIL import Image

from types import QAState

# TODO: algorithms
def get_algorithm(params):
    if params.job_type == JobType.ToolKit.value:
        return DummyToolKitAlgorithm(params)
    elif params.job_type == JobType.ItemKit.value:
        return DummyItemKitAlgorithm(params)
    elif params.job_type == JobType.QA.value:
        return Seeger_QA(params)

    else:
        print(f'invalid job type: {params.job_type}')


class TF_classifier:
    @abstractmethod
    def __init__(self, path):
        interpreter = tf.lite.Interpreter(model_path=path)
        interpreter.allocate_tensors()
        self.interpreter =  interpreter

    @abstractmethod
    def prep_image(self, sd_train):
        pass

    @abstractmethod
    def predict(self, img):
        input_details = self.interpreter.get_input_details()
        output_details = self.interpreter.get_output_details()

        self.interpreter.set_tensor(input_details[0]['index'], img)
        self.interpreter.invoke()
        
        return self.interpreter.get_tensor(output_details[0]['index'])

    @abstractmethod
    def adapt(self, result):
        pass

    @abstractmethod
    def run(self, img):
        #prepare the image
        prep_img = self.prep_image(img)
        #give the prepared image to the model and save its output
        result = self.predict(prep_img)
        #adapter function to make a uniqe value from the result
        return self.adapt(result)


class DummyToolKitAlgorithm:
    def __init__(self, params):
        self._params = params

    def run(self):
        pass

class DummyItemKitAlgorithm:
    def __init__(self, params):
        self._params = params

    def run(self):
        pass

class DummyQAAlgorithm:
    def __init__(self, params):
        self._params = params

    def run(self):
        pass

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


class Seeger_QA(TF_classifier):
    def __init__(self, params):
        super().__init__("seeger_yolov2.tflite")
        self.params = params    

    def prep_image(self, img):
        self.img = img
        img = cv2.resize(img, dsize=(640, 640), interpolation=cv2.INTER_CUBIC)
        img = img.reshape((1, img.shape[0], img.shape[1], img.shape[2]))
        img = img.astype(np.float32)
        img /= 255
        return img

    def predict(self, img):
        return super().predict(img)


    '''
    MAKE A BOOLEAN VALUE FROM THE MEAN OF THE DISTANCES BETWEEN KOORDINATES
    '''
    def adapt(self, result):
        xyxy, classes, scores = self.YOLOdetect(result) #boxes(x,y,x,y), classes(int), scores(float) [25200]

        koords = []
        for i in range(len(scores)):
                #treshold values 
                if ((scores[i] > 0.5) and (scores[i] <= 1.0)):
                    H = self.img.shape[0]
                    W = self.img.shape[1]
                    xmin = int(max(1,(xyxy[0][i] * W)))
                    ymin = int(max(1,(xyxy[1][i] * H)))
                    xmax = int(min(W,(xyxy[2][i] * W)))
                    ymax = int(min(H,(xyxy[3][i] * H)))

                    koords.append(((xmin,ymin), (xmax, ymax)))

        try:
            distances = self.dist_calc(koords)
        except:
            distances = [-1]

        return QAState.Success if np.mean(distances) < 200 else QAState.Failure if np.mean(distances) > 0 else QAState.Uncertain

    def run(self, img):
        return super().run(img)

    '''
    OUTPUT CONVERTER FUNCTIONS
    '''
    def classFilter(self, classdata):
        classes = ["seeger"]  # create a list
        for i in range(classdata.shape[0]):         # loop through all predictions
            classes.append(classdata[i].argmax())   # get the best classification location
        return classes  # return classes (int)

    def YOLOdetect(self, output_data):  # input = interpreter, output is boxes(xyxy), classes, scores
        output_data = output_data[0]                # x(1, 25200, 7) to x(25200, 7)
        boxes = np.squeeze(output_data[..., :4])    # boxes  [25200, 4]
        scores = np.squeeze( output_data[..., 4:5]) # confidences  [25200, 1]
        classes = self.classFilter(output_data[..., 5:]) # get classes
        # Convert nx4 boxes from [x, y, w, h] to [x1, y1, x2, y2] where xy1=top-left, xy2=bottom-right
        x, y, w, h = boxes[..., 0], boxes[..., 1], boxes[..., 2], boxes[..., 3] #xywh
        xyxy = [x - w / 2, y - h / 2, x + w / 2, y + h / 2]  # xywh to xyxy   [4, 25200]

        return xyxy, classes, scores