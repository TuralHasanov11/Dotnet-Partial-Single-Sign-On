<template>
  <header>
    <div class="wrapper">
      <template v-if="isAuthenticated">
        <a href="javascript:void(0)">{{ user?.name }}</a>
        <div class="d-flex">
          <button
            class="btn btn-danger mr-2"
            @click="
              async () => {
                await logout();
              }
            "
          >
            Logout
          </button>
          <button class="btn btn-info mx-2" @click="getProtectedData">Get Api Service's Protected Data</button>
          <button class="btn btn-success" @click="postProtectedData">Post to Api Service's Protected Endpoint</button>
        </div>
      </template>
      <template v-else>
        <div class="row">
          <form @submit.prevent="submitLoginForm" class="col-6 mb-3">
            <div class="form-group mb-3">
              <label class="form-label" for="login-email">Email</label>
              <input type="email" v-model="loginForm.email" placeholder="Email" id="login-email" class="form-control" />
            </div>
            <div class="form-group mb-3">
              <label class="form-label" for="login-password">Password</label>
              <input
                type="password"
                class="form-control"
                v-model="loginForm.password"
                placeholder="Password"
                autocomplete="off"
                for="login-password"
              />
            </div>
            <button class="btn btn-primary" type="submit">Login</button>
          </form>
          <button @click="loginWithGitHub()" class="btn btn-secondary">Login with GitHub</button>
        </div>
      </template>
    </div>
  </header>

  <RouterView />
</template>

<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router';
import useIdentity from '@/composables/useIdentity';
import { ref } from 'vue';
import useTestStore from './composables/test';

const { user, isAuthenticated, login, logout, loginWithGitHub } = useIdentity();
const testStore = useTestStore();

const loginForm = ref({ email: '', password: '' });

async function submitLoginForm() {
  await login(loginForm.value.email, loginForm.value.password);
}

async function getProtectedData() {
  const result = await testStore.getTest();
  alert(result);
}

async function postProtectedData() {
  const result = await testStore.postTest();
  alert(result);
}
</script>
