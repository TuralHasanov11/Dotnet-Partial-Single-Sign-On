import { anonymousUser, useIdentityStore } from '@/stores/identity';
import type { User } from '@/types/identity';
import { computed, type ComputedRef, type Ref } from 'vue';

export default function useIdentity(): {
  user: Ref<User | null>;
  isAuthenticated: ComputedRef<boolean>;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  register: (username: string, email: string, password: string, confirmPassword: string) => Promise<void>;
  loginWithGitHub: (returnUrl?: string) => void;
} {
  const identityStore = useIdentityStore();

  const user = computed<User | null>(() => identityStore.user);
  const isAuthenticated = computed<boolean>(() => user.value !== null && user.value.id !== anonymousUser.id);

  return {
    user,
    isAuthenticated,
    login: identityStore.login,
    logout: identityStore.logout,
    register: identityStore.register,
    loginWithGitHub: identityStore.loginWithGitHub,
  };
}
