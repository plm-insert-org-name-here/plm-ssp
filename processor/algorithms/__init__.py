from utils.types import JobType, QAState
from algorithms.dummy import DummyToolKitAlgorithm, DummyItemKitAlgorithm, DummyQAAlgorithm
from algorithms.item_kit import Item_kit_algo
from algorithms.qa import Seeger_QA

def get_algorithm(params, dummy):
    if params.job_type == JobType.ToolKit.value:
        if dummy:
            return DummyToolKitAlgorithm(params)
    elif params.job_type == JobType.ItemKit.value:
        if dummy:
            return DummyItemKitAlgorithm(params)
    elif params.job_type == JobType.QA.value:
        if dummy:
            return DummyQAAlgorithm(params)
        return Seeger_QA(params)

    else:
        print(f'invalid job type: {params.job_type}')
