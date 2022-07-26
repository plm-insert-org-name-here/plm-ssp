byteorder='little'

def to_bytes(number, n_bytes):
    return (number).to_bytes(n_bytes, byteorder)

def from_bytes(bs):
    return int.from_bytes(bs, byteorder)
