import api from './axios';

export const get = (url, config = {}) => {
  return api.get(url, config);
};

export const post = (url, data, config = {}) => {
  return api.post(url, data, config);
};

export const put = (url, data, config = {}) => {
  return api.put(url, data, config);
};

export const patch = (url, data, config = {}) => {
  return api.patch(url, data, config);
};

export const remove = (url, config = {}) => {
  return api.delete(url, config);
};
