import { createFetch } from '@vueuse/core'

const useAppFetch = createFetch({
  baseUrl: import.meta.env.VITE_API_URI,
  options: {},
  fetchOptions: {
    mode: 'cors',
    credentials: 'include',
  },
})

export default useAppFetch
