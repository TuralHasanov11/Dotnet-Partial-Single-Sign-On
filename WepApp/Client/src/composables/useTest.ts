import useAppFetch from '@/composables/useAppFetch';

interface Test {
  getTest: () => Promise<string>;
  postTest: () => Promise<string>;
}

export default function useTest(): Test {
  async function getTest(): Promise<string> {
    try {
      const { data } = await useAppFetch('/api/api-service/protected').get().json<{ message: string }>();

      if (data.value) {
        return data.value.message;
      }

      throw new Error('No data returned');
    } catch (error) {
      console.log(error);
      return 'Error';
    }
  }

  async function postTest(): Promise<string> {
    try {
      const { data } = await useAppFetch('/api/api-service/protected').post().json<{ message: string }>();

      if (data.value) {
        return data.value.message;
      }

      throw new Error('No data returned');
    } catch (error) {
      console.log(error);
      return 'Error';
    }
  }

  return {
    getTest,
    postTest,
  };
}
