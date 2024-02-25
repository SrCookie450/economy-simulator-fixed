import * as HTTPExceptions from 'ts-httpexceptions';

export default class StdExceptions {
    public NotFound = HTTPExceptions.NotFound;
    public BadRequest = HTTPExceptions.BadRequest;
    public InternalServerError = HTTPExceptions.InternalServerError;
}