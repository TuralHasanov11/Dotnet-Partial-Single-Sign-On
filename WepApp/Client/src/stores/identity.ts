import useAppFetch from '@/composables/useAppFetch';
import type { User } from '@/types/identity';
import { defineStore } from 'pinia';
import { ref } from 'vue';

export const anonymousUser: User = {
  id: '',
  name: '',
  email: '',
  roles: [],
};

export const useIdentityStore = defineStore('identity', () => {
  const user = ref<User | null>(anonymousUser);

  async function register(username: string, email: string, password: string, confirmPassword: string): Promise<void> {
    try {
      await useAppFetch('/api/identity-service/identity/register', {
        method: 'POST',
        body: JSON.stringify({
          username: username,
          email: email,
          password: password,
          confirmPassword: confirmPassword,
        }),
      });

      await getUserInfo();
    } catch (error) {
      console.log(error);
    }
  }

  async function login(email: string, password: string): Promise<void> {
    try {
      await useAppFetch('/api/identity-service/identity/login', {
        method: 'POST',
        body: JSON.stringify({
          email: email,
          password: password,
        }),
      });

      await getUserInfo();
    } catch (error) {
      console.log(error);
    }
  }

  async function logout(): Promise<void> {
    try {
      await useAppFetch('/api/identity-service/identity/logout', {
        method: 'POST',
        body: JSON.stringify({}),
      });

      user.value = null;
    } catch (error) {
      console.log(error);
    }
  }

  async function getUserInfo(): Promise<void> {
    const { data } = await useAppFetch<string>('/api/identity-service/identity/user-info');
    if (data.value != null) {
      user.value = JSON.parse(data.value) as User;
    }
  }

  return { user, getUserInfo, register, login, logout };
});
