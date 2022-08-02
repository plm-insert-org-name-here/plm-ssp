import json
from collections import namedtuple
from utils.misc import root_path
from utils.types import TemplateState, TemplateStateObj

Script = namedtuple('Script', 'name task_id update_period steps')
ScriptStep = namedtuple('ScriptStep', 'elapsed states')

scripts = {}

def load_scripts():
    global scripts

    folder = root_path / 'dummy_scripts' / 'active'
    files = [f for f in folder.iterdir() if f.suffix == '.json']
    for f in files:
        script = _parse_script(f)
        scripts[script.task_id] = script

def get_script(task_id):
    global scripts

    try:
        return scripts[task_id]
    except KeyError:
        return None

def _parse_script(path):
    j = None
    with open(str(path), 'r') as file:
        contents = file.read()
        j = json.loads(contents)

    try:
        task_id = j['taskId']
        update_period = j['updatePeriod']
        steps = [_parse_step(step) for step in j['steps']]
        return Script(path.stem, task_id, update_period, steps)
    except (KeyError, TypeError) as ex: 
        print('Failed to parse dummy script: ' + str(ex))
        return None

def _parse_step(s):
    try:
        elapsed = s['elapsed']
        states = [(state['id'], state['state']) for state in s['states']]
        states = [TemplateStateObj(id, TemplateState[state]) for (id, state) in states]
        return ScriptStep(elapsed, states)
    except (KeyError, TypeError) as ex:
        print('Failed to parse dummy script step: ' + str(ex))
        return None

