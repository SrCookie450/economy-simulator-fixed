import { readFileSync } from 'fs';
import { join } from 'path';
let cookies = readFileSync(join(__dirname, '../../cookies.txt')).toString().replace(/\r/g, '').split('\n').filter(val => {
    return !!val;
});
let index = 0;

let reserved: string[] = [];
export const get = (): { cookie: string; done: () => void } => {
    let val = cookies[index];
    if (typeof val !== 'string' || !val) {
        index = 0;
        val = cookies[0];
    }
    index++;
    if (reserved.includes(val)) {
        return get();
    }
    reserved.push(val);
    return {
        cookie: val,
        done: () => {
            reserved = reserved.filter(bad => {
                return bad !== val;
            })
        }
    };
}
export const bad = (badCookie: string) => {
    cookies = cookies.filter(val => {
        return val !== badCookie;
    });
}