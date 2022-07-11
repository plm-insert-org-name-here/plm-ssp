from utils.types import JobType
from time import sleep

# TODO: algorithms
def get_algorithm(params):
    if params.job_type == JobType.ToolKit.value:
        return DummyToolKitAlgorithm(params)
    elif params.job_type == JobType.ItemKit.value:
        return DummyItemKitAlgorithm(params)
    elif params.job_type == JobType.QA.value:
        return DummyQAAlgorithm(params)

    else:
        print(f'invalid job type: {job_type}')


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

