export const reportImageFail = ({ src, errorEvent, type }) => {
  console.error('[error] image load fail for', src, '\n\nevent data:', errorEvent, '\n', 'type', type);
}