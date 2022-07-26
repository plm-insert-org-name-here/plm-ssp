from algorithms.base import TF_classifier
import numpy as np
import cv2

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
