import * as matchers from '@testing-library/jest-dom/matchers';

expect.extend(matchers);

// jsdom does not implement the HTMLDialogElement modal API.
if (typeof HTMLDialogElement !== 'undefined') {
  HTMLDialogElement.prototype.showModal = function showModal() {
    this.setAttribute('open', '');
  };
  HTMLDialogElement.prototype.close = function close() {
    this.removeAttribute('open');
    this.dispatchEvent(new Event('close'));
  };
}
