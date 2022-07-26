from algorithms.base import TF_classifier
from utils.types import TemplateStateObj, TemplateState, ToolKitResult, ItemKitResult

class DummyToolKitAlgorithm(TF_classifier):
    def __init__(self, params):
        self._params = params

    def run(self, img):
        states = [TemplateStateObj(templ.id, TemplateState.Present) 
                for templ in self._params.templs]
        return ToolKitResult(states)

class DummyItemKitAlgorithm(TF_classifier):
    def __init__(self, params):
        self._params = params

    def run(self, img):
        states = [TemplateStateObj(templ.id, TemplateState.Present)
                for templ in self._params.templs]
        return ItemKitResult(states)

class DummyQAAlgorithm(TF_classifier):
    def __init__(self, params):
        self._params = params

    def run(self, img):
        pass
