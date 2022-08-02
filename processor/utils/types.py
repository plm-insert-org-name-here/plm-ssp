from collections import namedtuple
from utils.serialization import to_bytes
from enum import Enum

Template = namedtuple("Template", "id x y w h order")
Params = namedtuple("Params", "job_type templs")
TemplateStateObj = namedtuple("TemplateStateObj", "id inner")

# TODO: Result classes that know how to serialize themselves to socket
class KitResult:
    def __init__(self, states):
        self.states = states

    def serialize(self):
        print(self.states)
        buf = b'' 
        buf += to_bytes(len(self.states), 4)
        for state in self.states:
            buf += to_bytes(state.id, 4)
            buf += to_bytes(state.inner.value, 4)
        return buf

class QAResult:
    def __init__(self, result):
        self.result = result

    def serialize(self):
        return to_bytes(result, 4)

class JobType(Enum):
    ToolKit = 0
    ItemKit = 1
    QA = 2

class PacketType(Enum):
    Params = 0
    Frame = 1
    Stop = 2

class TemplateState(Enum):
    Present = 0
    Missing = 1
    Obstructed = 2

class QAState(Enum):
    Success = 0
    Failure = 1
    Uncertain = 2
