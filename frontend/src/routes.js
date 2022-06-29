let origin = window.ENV.backend;
const api = origin + "/api/v1";

export const Routes = {
    locations: api + "/locations",
    detectors: api + "/detectors",
};
