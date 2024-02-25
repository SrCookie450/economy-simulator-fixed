export const abbreviateNumber = value => {
  if (value < 1000) {
    return value.toLocaleString();
  }
  if (value < 1000000) {
    if (value > 99999) {
      return value.toString().slice(0, 3) + 'K+';
    }
    if (value > 9999) {
      return value.toString().slice(0, 2) + 'K+';
    }
  }
  var suffixes = ["", "k", "m", "b", "t"];
  var suffixNum = Math.floor(("" + value).length / 3);
  let shortValue = parseFloat((suffixNum != 0 ? (value / Math.pow(1000, suffixNum)) : value).toPrecision(2));
  if (shortValue % 1 != 0) {
    // @ts-ignore
    shortValue = shortValue.toFixed(1);
  }
  return shortValue + suffixes[suffixNum].toUpperCase() + '+';
}