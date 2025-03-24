
export const relation_vec = [];

// 6x6 的表格
const size = 6;

// 生成横向相邻关系
for (let y = 0; y < size; y++) {
    for (let x = 0; x < size - 1; x++) {
        relation_vec.push([x, y, x + 1, y]);
    }
}

// 生成纵向相邻关系
for (let x = 0; x < size; x++) {
    for (let y = 0; y < size - 1; y++) {
        relation_vec.push([x, y, x, y + 1]);
    }
}

console.log(relation_vec);
