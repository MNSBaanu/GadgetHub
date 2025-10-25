// Authentication utilities for all pages

// Session validation cache to avoid too many API calls
// Use window object to prevent redeclaration errors
if (!window.sessionValidationCache) {
    window.sessionValidationCache = {
        sessionId: null,
        isValid: false,
        timestamp: 0,
        cacheDuration: 300000 // 5 minute cache - less aggressive validation
    };
}

// Check if user is logged in (with smart validation)
async function isUserLoggedIn() {
    const sessionId = localStorage.getItem('sessionId');
    if (!sessionId) {
        return false;
    }
    
    // Check cache first
    const now = Date.now();
    if (window.sessionValidationCache.sessionId === sessionId && 
        window.sessionValidationCache.timestamp > now - window.sessionValidationCache.cacheDuration) {
        return window.sessionValidationCache.isValid;
    }
    
    try {
        const response = await fetch('https://localhost:7091/api/Auth/validate-session', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ sessionId: sessionId })
        });
        
        const result = await response.json();
        
        // Update cache
        window.sessionValidationCache = {
            sessionId: sessionId,
            isValid: result.success,
            timestamp: now
        };
        
        if (!result.success) {
            // Session is invalid, clear localStorage
            localStorage.removeItem('sessionId');
            localStorage.removeItem('userData');
            return false;
        }
        
        return true;
    } catch (error) {
        console.error('Session validation error:', error);
        // On error, assume session is valid to avoid unnecessary logouts
        // This prevents network issues from logging users out
        return true;
    }
}

// Simple check for navigation (no server validation)
function isUserLoggedInSimple() {
    const sessionId = localStorage.getItem('sessionId');
    const userData = localStorage.getItem('userData');
    return sessionId !== null && userData !== null;
}

// Check if admin is logged in
function isAdminLoggedIn() {
    return localStorage.getItem('adminSession') !== null;
}

// Get user data from localStorage
function getUserData() {
    const userData = localStorage.getItem('userData');
    return userData ? JSON.parse(userData) : null;
}

// Get admin data from localStorage
function getAdminData() {
    const adminData = localStorage.getItem('adminUser');
    return adminData ? JSON.parse(adminData) : null;
}

// Update navigation based on login status
function updateNavigation() {
    checkUserLoginStatus();
}

// Manual navigation update function for debugging
function forceUpdateNavigation() {
    console.log('🔄 Force updating navigation...');
    checkUserLoginStatus();
}

// Check user login status and show/hide profile (fast navigation)
function checkUserLoginStatus() {
    console.log('🔍 Checking user login status...');
    const adminSession = localStorage.getItem('adminSession');
    const sessionId = localStorage.getItem('sessionId');
    const userData = localStorage.getItem('userData');
    
    console.log('Admin session:', adminSession);
    console.log('User session ID:', sessionId);
    console.log('User data:', userData);
    
    const userProfileNav = document.getElementById('userProfileNav');
    const adminProfileNav = document.getElementById('adminProfileNav');
    const loginLink = document.getElementById('loginLink');
    
    console.log('Elements found:', {
        userProfileNav: !!userProfileNav,
        adminProfileNav: !!adminProfileNav,
        loginLink: !!loginLink
    });
    
    if (adminSession && adminProfileNav && loginLink) {
        // Admin is logged in
        try {
            const admin = JSON.parse(localStorage.getItem('adminUser'));
            updateAdminAvatar(admin);
            adminProfileNav.style.display = 'flex';
            loginLink.style.display = 'none';
            if (userProfileNav) userProfileNav.style.display = 'none';
        } catch (error) {
            console.error('Error parsing admin data:', error);
            adminProfileNav.style.display = 'none';
            loginLink.style.display = 'flex';
        }
    } else if (isUserLoggedInSimple() && userProfileNav && loginLink) {
        // User appears to be logged in (fast check for navigation)
        console.log('✅ User appears to be logged in, updating navigation...');
        try {
            const userData = localStorage.getItem('userData');
            const user = JSON.parse(userData);
            console.log('User data parsed:', user);
            updateUserAvatar(user);
            userProfileNav.style.display = 'flex';
            loginLink.style.display = 'none';
            if (adminProfileNav) adminProfileNav.style.display = 'none';
            console.log('✅ User navigation updated successfully');
        } catch (error) {
            console.error('❌ Error parsing user data:', error);
            userProfileNav.style.display = 'none';
            loginLink.style.display = 'flex';
        }
    } else if (userProfileNav && adminProfileNav && loginLink) {
        // No one is logged in
        console.log('❌ No one is logged in, showing login link');
        userProfileNav.style.display = 'none';
        adminProfileNav.style.display = 'none';
        loginLink.style.display = 'flex';
    } else {
        console.log('❌ Navigation elements not found or user not logged in');
        console.log('isUserLoggedInSimple():', isUserLoggedInSimple());
        console.log('Elements available:', {
            userProfileNav: !!userProfileNav,
            adminProfileNav: !!adminProfileNav,
            loginLink: !!loginLink
        });
    }
}

// Update user avatar with initials
function updateUserAvatar(user) {
    const avatarInitialsElement = document.getElementById('avatarInitials');
    
    if (avatarInitialsElement && user) {
        const firstName = user.firstName || 'U';
        const lastName = user.lastName || '';
        const initials = (firstName.charAt(0) + (lastName.charAt(0) || '')).toUpperCase().substring(0, 2);
        avatarInitialsElement.textContent = initials;
    }
}

// Update admin avatar with initials
function updateAdminAvatar(admin) {
    const avatarInitialsElement = document.getElementById('adminAvatarInitials');
    
    if (avatarInitialsElement && admin) {
        avatarInitialsElement.textContent = 'A'; // Always 'A' for Admin
    }
}

// Clear session validation cache
function clearSessionCache() {
    window.sessionValidationCache = {
        sessionId: null,
        isValid: false,
        timestamp: 0,
        cacheDuration: 60000
    };
}

// Logout function for regular users
function logout() {
    const sessionId = localStorage.getItem('sessionId');
    if (sessionId) {
        fetch('https://localhost:7091/api/Auth/logout', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ sessionId: sessionId })
        }).catch(error => {
            console.error('Logout API call failed:', error);
        });
    }
    
    localStorage.removeItem('sessionId');
    localStorage.removeItem('userData');
    clearSessionCache();
    window.location.href = '/Index';
}

// Logout function for admin
function adminLogout() {
    const adminSession = localStorage.getItem('adminSession');
    if (adminSession) {
        fetch('/api/admin/logout', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ sessionId: adminSession })
        }).catch(error => {
            console.error('Admin logout API call failed:', error);
        });
    }
    
    localStorage.removeItem('adminSession');
    localStorage.removeItem('adminUser');
    clearSessionCache();
    window.location.href = '/Index';
}

// Protect pages that require authentication
async function protectPage() {
    const sessionId = localStorage.getItem('sessionId');
    const adminSession = localStorage.getItem('adminSession');
    
    if (!sessionId && !adminSession) {
        // No session found, redirect to login
        window.location.href = '/Login';
        return false;
    }
    
    if (adminSession) {
        // Admin is logged in, allow access
        return true;
    }
    
    // Validate user session with server
    const userLoggedIn = await isUserLoggedIn();
    if (!userLoggedIn) {
        // Session is invalid, redirect to login
        window.location.href = '/Login';
        return false;
    }
    
    return true;
}

// Initialize authentication on page load
document.addEventListener('DOMContentLoaded', function() {
    updateNavigation();
});
