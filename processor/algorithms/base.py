from abc import abstractmethod
import tensorflow as tf

class TF_classifier:
    @abstractmethod
    def __init__(self, path):
        interpreter = tf.lite.Interpreter(model_path=path)
        interpreter.allocate_tensors()
        self.interpreter = interpreter

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

    @abstractmethod
    def update_params(self, params):
        self.params = params
