// Similar to Array.prototype.filter(), but captures the items that don't pass the filter
// into a secondary array
export const bifilter = (f, xs) => {
    return xs.reduce(
        ([T, F], x, i, arr) => {
            if (f(x, i, arr)) return [[...T, x], F];
            else return [T, [...F, x]];
        },
        [[], []]
    );
};
