// creates a N x 18 size array
function numpyfySeq(Sequence) {
    'use strict';
    var transpose = m => m[0].map((x, i) => m.map(x => x[i]));
    var array = [];
    for (var feature in Sequence) {
        if (feature != "time") {
            array.push(Sequence[feature])
        }
    }
    return transpose(array);
}