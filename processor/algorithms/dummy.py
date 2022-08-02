from algorithms.base import TF_classifier
from utils.types import TemplateStateObj, TemplateState, KitResult
from utils.misc import root_path
from time import sleep, time
from pathlib import Path
from collections import namedtuple
import json

Script = namedtuple('Script', 'update_period steps')
ScriptStep = namedtuple('ScriptStep', 'elapsed states')

def parse_script(filename):
    if (not filename.endswith('.json')):
        filename += '.json'
    full_filename = Path(root_path / 'dummy_scripts' / filename)
    if not full_filename.exists():
        print(f'Dummy script file {full_filename} does not exist!')
        return None

    j = None
    with open(str(full_filename), 'r') as file:
        contents = file.read()
        j = json.loads(contents)

    try:
        update_period = j['updatePeriod']
        steps = [parse_step(step) for step in j['steps']]
        return Script(update_period, steps)
    except (KeyError, TypeError) as ex: 
        print('Failed to parse dummy script: ' + str(ex))
        return None

def parse_step(s):
    try:
        elapsed = s['elapsed']
        states = [(state['id'], state['state']) for state in s['states']]
        states = [TemplateStateObj(id, TemplateState[state]) for (id, state) in states]
        return ScriptStep(elapsed, states)
    except (KeyError, TypeError) as ex:
        print('Failed to parse dummy script step: ' + str(ex))
        return None

class DummyKitAlgorithm(TF_classifier):
    def __init__(self, params, dummy):
        self._params = params
        self._dummy = dummy
        self._elapsed = None
        self._script = None

    def _get_current_state(self):
        self._elapsed += self._script.update_period
        return [st for st in self._script.steps if st.elapsed < self._elapsed][-1].states

    def run(self, img):
        if self._dummy is False:
            states = [TemplateStateObj(templ.id, TemplateState.Present) 
                    for templ in self._params.templs]
            sleep(0.2)
            return KitResult(states)
        else:
            if self._script is None:
                self._elapsed = 0
                self._script = parse_script(self._dummy)
                if not self._script:
                    return None

            t = time()
            t += self._script.update_period / 1000

            current_states = self._get_current_state()

            sleep(t - time())
            print(current_states)
            return KitResult(current_states)

class DummyQAAlgorithm(TF_classifier):
    def __init__(self, params):
        self._params = params

    def run(self, img):
        pass
