from collections import namedtuple
from enum import Enum

Template = namedtuple("Template", "id x y w h order")
Params = namedtuple("Params", "job_type templs")

# TODO: Result classes that know how to serialize themselves to socket
ToolKitResult = namedtuple("ToolKitResult", "states")
ItemKitResult = namedtuple("ItemKitResult", "states")
QAResult = namedtuple("QAResult", "state")

class JobType(Enum):
    ToolKit = 0
    ItemKit = 1
    QA = 2

class PacketType(Enum):
    Params = 0
    Frame = 1

class TemplateState(Enum):
    Present = 0
    Missing = 1
    Obstructed = 2

class QAState(Enum):
    Success = 0
    Failure = 1
    Uncertain = 2