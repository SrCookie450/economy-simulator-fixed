import { useState } from "react";
import { logger } from "../../../lib/logger";
import { getUrlWithProxy } from "../../../lib/request";
import { getBaseUrl } from "../../../lib/request";

const css = `
a.list-header {
  display: inline-block;
}
img.header-thumb, img.avatar-card-image {
  height: 85px;
  width: auto;
  display: inline-block;
}
ul.vlist.feeds {
  padding: 0;
  list-style: none;
}
div.list-body {
  display: inline-block;
  margin-left: 10px;
  max-width: calc(100% - 100px);
}
html,body {
  font-family: "Source Sans Pro", serif;
}
p {
  padding: 0;
  margin: 0;
}
p.feedtext {
  padding: 10px 0;
}
span.xsmall {
  color: #777;
  font-size: 12px;
}
a {
  color: #00a2ff;
  text-decoration: none;
}
a:visited {
  color: #00a2ff;
}
li.list-item {
  padding-bottom: 20px;
  border-bottom: 1px solid #c3c3c3;
  padding-top: 10px;
}
`;

const MyFeed = props => {
  const [height, setHeight] = useState('auto');
  // TODO: would a flag be better? I guess automatically guessing is more convenient...
  const shouldProxyRequest = getBaseUrl().startsWith(window.location.protocol + '//' + window.location.hostname) === false;
  const feedFrameUrl = shouldProxyRequest ? (getUrlWithProxy(getBaseUrl() + '/Feeds/GetUserFeed')) : getBaseUrl() + '/Feeds/GetUserFeed';

  return <div>
    <iframe id='homepage-iframe-feed' height='100%' scrolling='no' style={{ width: '100%', height: height, overflow: 'hidden' }} src={feedFrameUrl} onLoad={() => {
      logger.info('feed', 'feed iframe loaded');
      // @ts-ignore
      const current = document.getElementById('homepage-iframe-feed');
      // @ts-ignore
      const doc = current.contentWindow.document;
      const body = doc.body;
      const font = doc.createElement('link');
      font.setAttribute('href', 'https://fonts.googleapis.com/css2?family=Source+Sans+Pro:ital,wght@0,200;0,300;0,400;0,600;0,700;0,900;1,200;1,300;1,400;1,600;1,700;1,900&amp;display=swap');
      font.setAttribute('rel', 'stylesheet');
      doc.body.appendChild(font);
      const styles = doc.createElement('style');
      styles.innerText = css;
      // doc.appendElement(styles)
      doc.body.appendChild(styles)
      const height = body.scrollHeight + 10;
      setHeight(height + 'px');
    }}></iframe>
  </div>
}

export default MyFeed;