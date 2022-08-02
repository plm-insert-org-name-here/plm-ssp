from utils.types import JobType, QAState
from algorithms.dummy import DummyKitAlgorithm, DummyQAAlgorithm
from algorithms.item_kit import Item_kit_algo
from algorithms.qa import Seeger_QA

def get_algorithm(params, dummy):
    if params.job_type == JobType.ToolKit.value:
        if dummy is not None:
            return DummyKitAlgorithm(params, dummy)
        # TODO: return real algorithm here
    elif params.job_type == JobType.ItemKit.value:
        if dummy is not None:
            return DummyKitAlgorithm(params, dummy)
        # TODO: return real algorithm here
    elif params.job_type == JobType.QA.value:
        if dummy is not None:
            return DummyQAAlgorithm(params, dummy)
        return Seeger_QA(params)

    else:
        print(f'invalid job type: {params.job_type}')
