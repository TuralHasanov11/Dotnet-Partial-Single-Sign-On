import { useIdentityStore } from '@/stores/identity';

export default {
  async install() {
    const identity = useIdentityStore();
    await identity.getUserInfo();
  },
};
