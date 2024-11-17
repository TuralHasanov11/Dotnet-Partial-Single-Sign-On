import { anonymousUser, useIdentityStore } from '@/stores/identity';
import type { User } from '@/types/identity';
import { computed } from 'vue';

export default function useIdentity() {
  const identityStore = useIdentityStore();

  const user = computed<User | null>(() => identityStore.user);
  const isAuthenticated = computed<boolean>(() => user.value !== null && user.value.id !== anonymousUser.id);

  function hasRole(role: string): boolean {
    return user.value?.roles?.some((r) => r === role) ?? false;
  }

  return {
    user,
    isAuthenticated,
    hasRole,
    login: identityStore.login,
    logout: identityStore.logout,
    register: identityStore.register,
  };
}
