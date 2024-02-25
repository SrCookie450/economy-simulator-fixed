/**
 * Runtime type enforcer. If the argument does not match the expected type, a default value is returned.
 */
const t = {
  string: (str) => {
    if (typeof str !== 'string') {
      return '';
    }
    return str;
  },
  array: (arr) => {
    if (!arr || !Array.isArray(arr)) {
      return [];
    }
    return arr;
  },
  object: (obj) => {
    if (typeof obj !== 'object' && !obj) {
      return {}
    }
    return obj;
  },
}

export default t;