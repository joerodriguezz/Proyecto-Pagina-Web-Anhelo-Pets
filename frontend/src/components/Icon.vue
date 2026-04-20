<script setup>
import { computed } from 'vue'
import * as Boxicons from '@boxicons/vue'

const props = defineProps({
  name: { type: String, required: false },
  size: { type: [Number, String], default: 18 },
  fallback: { type: String, default: '' }
})

const Comp = computed(() => {
  if (!props.name) return null
  return Boxicons[props.name] || null
})
</script>

<template>
  <component v-if="Comp" :is="Comp" class="boxicon" :style="{ width: size + 'px', height: size + 'px' }" />
  <span v-else class="icon-fallback">{{ fallback }}</span>
</template>

<style scoped>
.boxicon { display:inline-flex; align-items:center; justify-content:center; vertical-align:middle; }
.icon-fallback { display:inline-flex; align-items:center; justify-content:center; font-size:14px; }
</style>
