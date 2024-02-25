import dayjs from 'dayjs';
import format from 'dayjs/plugin/advancedFormat';
dayjs.extend(format);
import tz from 'dayjs/plugin/timezone';
dayjs.extend(tz);
import relative from 'dayjs/plugin/relativeTime';
dayjs.extend(relative);
import customParseFormat from 'dayjs/plugin/customParseFormat';
dayjs.extend(customParseFormat);

export default dayjs;