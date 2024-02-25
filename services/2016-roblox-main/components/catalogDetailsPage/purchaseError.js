export default class PurchaseError extends Error {
  constructor(errorState) {
    super('Purchase failed with state ' + errorState);
    this.state = errorState;
  }
}