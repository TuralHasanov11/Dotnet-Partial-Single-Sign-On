import { useIdentityStore } from '@/stores/identity';

export default {
  async install() {
    const { getUserInfo } = useIdentityStore();
    await getUserInfo();
  },
};
