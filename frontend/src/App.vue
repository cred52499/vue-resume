<script setup lang="ts">
import { ref, onMounted } from 'vue'

// 2. 宣告響應式狀態，指定為 number 型別
const visitorCount = ref<number>(0)

// API 基礎路徑
// 自動判斷：如果在本地端就連本機後端，如果在線上就連 Render
const API_URL = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1'
  ? 'http://localhost:5286/api/visit'                    // 👈 本機開發測試
  : 'https://vue-resume-xlvc.onrender.com/api/visit';   // 👈 上線給別人看

// 1. 修改 Vue 中的 recordVisit 函式
const recordVisit = async (isNewSession: boolean = true): Promise<void> => {
  try {
    // 新 Session 才發送 POST（增加次數），否則發送 GET（只抓總數）
    const method = isNewSession ? 'POST' : 'GET'
    
    const response = await fetch(API_URL, {
      method: method,
      headers: { 'Content-Type': 'application/json' }
    })
    
    if (response.ok) {
      const data = await response.json()
      visitorCount.value = data.totalVisits // 寫入數字
    }
  } catch (error) {
    console.error('API 呼叫失敗:', error)
  }
}

// activeCardIndex: 0 = Personal, 1 = Skills, 2 = Achievements
const activeCardIndex = ref(0)
const totalCards = 3

// Expanded Card State
const isExpanded = ref(false)

const toggleExpand = (event: MouseEvent) => {
  // Prevent card click/swipe from interfering with the toggle button click
  event.stopPropagation()
  isExpanded.value = !isExpanded.value
}

// Calculates visual depth (0 = Front/Active, 1 = Middle, 2 = Back)
const getDepth = (cardIndex: number) => {
  return (cardIndex - activeCardIndex.value + totalCards) % totalCards
}

// Navigation helpers (Resets expand state on navigation)
const setCard = (index: number) => {
  if (activeCardIndex.value !== index) {
    isExpanded.value = false // Auto-reset when switching cards
    activeCardIndex.value = index
  }
}

const nextCard = () => {
  isExpanded.value = false // Auto-reset
  activeCardIndex.value = (activeCardIndex.value + 1) % totalCards
}

const prevCard = () => {
  isExpanded.value = false // Auto-reset
  activeCardIndex.value = (activeCardIndex.value - 1 + totalCards) % totalCards
}

/* --- Swipe / Drag Handling --- */
let startY = 0
let isDragging = false

const handleTouchStart = (e: TouchEvent) => {
  startY = e.touches?.[0]?.clientY ?? 0
}

const handleTouchEnd = (e: TouchEvent) => {
  const endY = e.changedTouches?.[0]?.clientY ?? startY
  handleSwipeGesture(startY, endY)
}

const handleMouseDown = (e: MouseEvent) => {
  startY = e.clientY
  isDragging = true
}

const handleMouseUp = (e: MouseEvent) => {
  if (!isDragging) return
  isDragging = false
  handleSwipeGesture(startY, e.clientY)
}

const handleSwipeGesture = (start: number, end: number) => {
  const diffY = start - end
  const threshold = 40

  if (Math.abs(diffY) > threshold) {
    if (diffY > 0) {
      nextCard()
    } else {
      prevCard()
    }
  }
}

// 2. 生命週期鉤子
onMounted(() => {
  const hasVisited = sessionStorage.getItem('visited')

  if (!hasVisited) {
    // 第一次開分頁：新增紀錄並寫入數字
    recordVisit(true)
    sessionStorage.setItem('visited', 'true')
  } else {
    // 按 F5 重新整理：不新增紀錄，但去後端拿目前的總數字
    recordVisit(false)
  }
})
</script>

<template>
  <div class="page-container">
    <header class="title">
      <h1>我的履歷</h1>
    </header>

    <!-- Card Stack Container -->
    <div 
      class="stack-container"
      @touchstart="handleTouchStart"
      @touchend="handleTouchEnd"
      @mousedown="handleMouseDown"
      @mouseup="handleMouseUp"
    >
      
      <!-- Card 1: Personal Details -->
      <div 
        class="card card-personal"
        :class="[`depth-${getDepth(0)}`, { active: activeCardIndex === 0 }]"
        @click="activeCardIndex = 0"
      >
        <div class="card-header">
          <span class="badge">01</span>
          <h2>個人資料</h2>
        </div>
        
        <div class="profile-layout">
          <img 
            src="/selfie.png" 
            alt="Profile Picture" 
            class="profile-pic"
          />
          <div class="info-list">
            <h3>劉曦鴻</h3>
            <p><strong>職稱:</strong> 軟體工程師</p>
            <p><strong>地址:</strong> 桃園市中壢區</p>
            <p><strong>郵箱:</strong> darren52499@gmail.com</p>
            <p><strong>電話:</strong> +886 932 269 556</p>
          </div>
        </div>
      </div>

      <!-- Card 2: Skills -->
      <div 
        class="card card-skills"
        :class="[`depth-${getDepth(1)}`, { expanded: activeCardIndex === 1 && isExpanded }]"
        @click="setCard(1)"
      >
        <div class="card-header">
          <span class="badge">02</span>
          <h2>專長</h2>

          <button 
            v-if="activeCardIndex === 1" 
            class="expand-btn"
            @click="toggleExpand"
            :title="isExpanded ? 'Collapse card' : 'Expand card'"
          >
          {{ isExpanded ? '✕' : '⤢' }}
          </button>
        </div>
        <div class="info-list">
          <p><strong>程式語言：</strong> Python、Java、C#、C、Golang、JavaScript、HTML/CSS</p>
          <p><strong>網頁與框架：</strong> Django、Vue.js、JSP</p>
          <p><strong>資料庫與 DevOps：</strong> PostgreSQL、MySQL、Docker、Git / GitHub</p>
          <p><strong>AI 與自動化：</strong> LLM 應用整合、提示工程（Prompt Engineering）、GitHub Copilot</p>
          <p><strong>行動與聊天機器人開發：</strong> Android Studio、LINE Bot API</p>
          <p><strong>工具與設計：</strong> Figma、MS Office（Excel、Word、PowerPoint）</p>
        </div>
      </div>

      <!-- Card 3: Achievements -->
      <div 
        class="card card-achievements"
        :class="[`depth-${getDepth(2)}`, { active: activeCardIndex === 2 }]"
        @click="activeCardIndex = 2"
      >
        <div class="card-header">
          <span class="badge">03</span>
          <h2>榮譽 / 獎狀</h2>
        </div>
        <div class="info-list">
          <p><strong>競賽：</strong></p>
          <p>2025 中原大學 AI 教與學標杆課程實踐暨成果展演競賽專案 學生組 特優獎</p>
          <p>2024 城市儀表板大黑客松</p>
          <p><strong>獎狀：</strong></p>
          <p>大學畢學業成績優良 第六名 / 資管系畢業專題成果 第一名</p>
          <p>僑務委員會獎狀 / 獎學金</p>
        </div>
      </div>

    </div>

    <div class="visitor-badge">
      瀏覽次數：<span>{{ visitorCount !== null ? visitorCount : '載入中...' }}</span>
    </div>
  </div>
</template>

<style scoped>
.page-container {
  width: 100vw;
  height: 100vh;
  height: 100dvh;
  position: fixed;
  top: 0;
  left: 0;
  overflow: hidden;
  
  display: flex;
  flex-direction: column;
  align-items: center;
  background-color: #0f172a;
  color: #f8fafc;
  font-family:
    "Segoe UI",
    "Microsoft JhengHei",
    "Microsoft YaHei",
    "Noto Sans CJK TC",
    sans-serif;
  padding: 20px;
  user-select: none;
  touch-action: none;
  justify-content: flex-start;
  padding-top: 40px;
}

.title {
  text-align: center;
  margin-bottom: 100px;
}

.title h1 {
  font-size: 2.2rem;
  margin: 0;
  color: #38bdf8;
}

/* Floating Bottom Right Visitor Badge */
.visitor-badge {
  position: fixed;
  bottom: 20px;
  right: 20px;
  z-index: 100;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  background: rgba(15, 23, 42, 0.85);
  backdrop-filter: blur(8px);
  border: 1px solid rgba(56, 189, 248, 0.3);
  color: #94a3b8;
  font-size: 0.85rem;
  padding: 8px 14px;
  border-radius: 20px;
  letter-spacing: 0.5px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.4);
}

.visitor-badge span {
  color: #38bdf8;
  font-weight: 700;
}

/* Stack Container */
.stack-container {
  position: relative;
  width: 100%;
  max-width: 500px;
  height: 320px;
  touch-action: none;
}

/* Base Card Styling */
.card {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  border-radius: 16px;
  padding: 28px;
  box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.5);
  cursor: pointer;
  transition: all 0.4s cubic-bezier(0.25, 1, 0.5, 1);
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
}

/* EXPANDED CARD STATE */
.card.expanded {
  top: -80px !important;
  height: 400px !important;
  transform: translateY(0) scale(1) !important;
  z-index: 50 !important;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.9);
}

.expand-btn {
  display: none;
  background: rgba(255, 255, 255, 0.15);
  border: 1px solid rgba(255, 255, 255, 0.2);
  color: #fff;
  border-radius: 8px;
  width: 32px;
  height: 32px;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  font-size: 1rem;
  transition: background 0.2s ease, transform 0.2s ease;
}

@media (max-width: 640px) {
  .expand-btn {
    display: flex;
  }
}

.expand-btn:hover {
  background: rgba(255, 255, 255, 0.3);
  transform: scale(1.1);
}

.scrollable-content {
  overflow-y: auto;
  touch-action: pan-y;
  padding-right: 6px;
}

.scrollable-content::-webkit-scrollbar {
  width: 5px;
}

.scrollable-content::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.2);
  border-radius: 4px;
}

/* Individual Card Colors */
.card-personal {
  background-color: #1e293b;
  border: 1px solid #334155;
}

.card-skills {
  background-color: #1e1b4b;
  border: 1px solid #312e81;
}

.card-achievements {
  background-color: #064e3b;
  border: 1px solid #065f46;
}

/* Dynamic Depth Classes */
.card.depth-0 {
  transform: translateY(-10px) scale(1.02);
  z-index: 10;
  box-shadow: 0 20px 35px -5px rgba(0, 0, 0, 0.7);
}

.card.depth-1 {
  transform: translateY(30px) scale(0.95);
  z-index: 5;
  opacity: 0.9;
}

.card.depth-2 {
  transform: translateY(60px) scale(0.9);
  z-index: 1;
  opacity: 0.8;
}

/* Card Inner Styling */
.card-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 20px;
}

.badge {
  background: rgba(255, 255, 255, 0.1);
  padding: 4px 10px;
  border-radius: 20px;
  font-size: 0.85rem;
  font-weight: bold;
}

.card-header h2 {
  margin: 0;
  font-size: 1.4rem;
}

.profile-layout {
  display: flex;
  gap: 20px;
  align-items: center;
}

.profile-pic {
  width: 100px;
  height: 100px;
  border-radius: 50%;
  object-fit: cover;
  border: 3px solid #38bdf8;
}

.info-list {
  flex: 1;
  overflow-y: hidden;
}

.info-list h3 {
  margin: 0 0 8px 0;
  font-size: 1.2rem;
  color: #38bdf8;
}

.info-list p {
  margin: 4px 0;
  font-size: 0.9rem;
  font-weight: 200;
  color: #cbd5e1;
}

.info-list p strong {
  font-weight: 700;
}

.placeholder-content {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 2px dashed rgba(255, 255, 255, 0.15);
  border-radius: 8px;
  color: #94a3b8;
}
</style>