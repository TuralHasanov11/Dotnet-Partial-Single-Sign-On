<template>
  <header>
    <img alt="Vue logo" class="logo" src="@/assets/logo.svg" width="125" height="125" />

    <div class="wrapper">
      <nav>
        <RouterLink to="/">Home</RouterLink>
        <RouterLink to="/about">About</RouterLink>
        <template v-if="identity.isAuthenticated.value">
          <a href="javascript:void(0)">{{ identity.user.value.name }}</a>
          <button @click="logout">Logout</button>
        </template>
        <template v-else>
          <form @submit.prevent="login">
            <input type="text" v-model="loginForm.email" placeholder="Email" />
            <input type="password" v-model="loginForm.password" placeholder="Password" />
            <button type="submit">Login</button>
          </form>
          <form @submit.prevent="register">
            <input type="text" v-model="registerForm.email" placeholder="Email" />
            <input type="password" v-model="registerForm.password" placeholder="Password" />
            <button type="submit">Register</button>
          </form>
        </template>
      </nav>
    </div>
  </header>

  <RouterView />
</template>

<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router';
import useIdentity from '@/composables/useIdentity';
import { ref } from 'vue';

const identity = useIdentity();

console.log(identity.isAuthenticated);

const loginForm = ref({ email: '', password: '' });
const registerForm = ref({ email: '', password: '' });

async function login() {
  await identity.login(loginForm.value);
}

async function register() {
  await identity.register(registerForm.value);
}

async function logout() {
  await identity.logout();
}
</script>

<style scoped>
header {
  line-height: 1.5;
  max-height: 100vh;
}

.logo {
  display: block;
  margin: 0 auto 2rem;
}

nav {
  width: 100%;
  font-size: 12px;
  text-align: center;
  margin-top: 2rem;
}

nav a.router-link-exact-active {
  color: var(--color-text);
}

nav a.router-link-exact-active:hover {
  background-color: transparent;
}

nav a {
  display: inline-block;
  padding: 0 1rem;
  border-left: 1px solid var(--color-border);
}

nav a:first-of-type {
  border: 0;
}

@media (min-width: 1024px) {
  header {
    display: flex;
    place-items: center;
    padding-right: calc(var(--section-gap) / 2);
  }

  .logo {
    margin: 0 2rem 0 0;
  }

  header .wrapper {
    display: flex;
    place-items: flex-start;
    flex-wrap: wrap;
  }

  nav {
    text-align: left;
    margin-left: -1rem;
    font-size: 1rem;

    padding: 1rem 0;
    margin-top: 1rem;
  }
}
</style>
