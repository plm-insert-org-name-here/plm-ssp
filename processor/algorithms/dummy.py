from algorithms.base import TF_classifier
from utils.types import TemplateStateObj, TemplateState, KitResult
from time import sleep, time
from pathlib import Path
from utils.dummy_script_loader import get_script
import json

class DummyKitAlgorithm(TF_classifier):
    def __init__(self, params, task_id):
        self._params = params
        self._elapsed = None 
        self._script = get_script(task_id)
        self._first_run = True

    def _get_current_state(self):
        self._elapsed += self._script.update_period
        return [st for st in self._script.steps if st.elapsed < self._elapsed][-1].states

    def run(self, img):
        if self._script is None:
            states = [TemplateStateObj(templ.id, TemplateState.Present) 
                    for templ in self._params.templs]
            sleep(0.2)
            return KitResult(states)

        if self._first_run:
            self._elapsed = 0
            self._first_run = False
            print(f'running dummy script {self._script.name}')

        t = time()
        t += self._script.update_period / 1000

        current_states = self._get_current_state()

        sleep(t - time())
        print(current_states)
        return KitResult(current_states)

    def update_params(self, params):
        self._params = params

# TODO
class DummyQAAlgorithm(TF_classifier):
    def __init__(self, params, task_id):
        self._params = params

    def run(self, img):
        pass

    def update_params(self, params):
        pass
